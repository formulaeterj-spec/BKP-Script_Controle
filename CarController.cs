using UnityEngine;

/// <summary>
/// Controlador de carro para volante + pedais físicos VINIK, usando WheelColliders.
/// 
/// PRÉ-REQUISITOS NA CENA:
/// - Um GameObject "Carro" com Rigidbody
/// - 4 WheelColliders (rodas dianteiras esquerda/direita e traseiras esquerda/direita)
/// - Opcionalmente, 4 objetos 3D (malhas) das rodas para acompanhar visualmente os WheelColliders
/// 
/// Os nomes dos eixos são campos públicos configuráveis no Inspector,
/// pois cada volante/marca pode mapear os eixos de forma diferente.
/// Use o script WheelAxisDebugger para descobrir os nomes corretos antes de configurar aqui.
/// </summary>
public class CarController : MonoBehaviour
{
    [Header("Eixos de Input (descubra com o WheelAxisDebugger)")]
    [Tooltip("Nome do eixo do volante (giro esquerda/direita). Ex: 'Joy1 Axis 1'")]
    public string eixoVolante = "Joy1 Axis 1";

    [Tooltip("Nome do eixo do pedal de acelerador. Ex: 'Joy1 Axis 2'")]
    public string eixoAcelerador = "Joy1 Axis 2";

    [Tooltip("Nome do eixo do pedal de freio. Ex: 'Joy1 Axis 3'")]
    public string eixoFreio = "Joy1 Axis 3";

    [Header("WheelColliders - Rodas Dianteiras (fazem a curva)")]
    public WheelCollider rodaDianteiraEsquerda;
    public WheelCollider rodaDianteiraDireita;

    [Header("WheelColliders - Rodas Traseiras (fazem a tração)")]
    public WheelCollider rodaTraseiraEsquerda;
    public WheelCollider rodaTraseiraDireita;

    [Header("Malhas visuais das rodas (opcional, para girar visualmente)")]
    public Transform malhaRodaDianteiraEsquerda;
    public Transform malhaRodaDianteiraDireita;
    public Transform malhaRodaTraseiraEsquerda;
    public Transform malhaRodaTraseiraDireita;

    [Header("Configurações de Direção")]
    [Tooltip("Ângulo máximo de esterço das rodas dianteiras, em graus")]
    public float anguloMaximoDeEsterço = 30f;

    [Header("Configurações de Motor e Freio")]
    [Tooltip("Torque máximo aplicado nas rodas traseiras ao acelerar")]
    public float torqueMaximoDoMotor = 1500f;

    [Tooltip("Força máxima de frenagem aplicada nas 4 rodas")]
    public float forcaMaximaDeFreio = 3000f;

    [Tooltip("Torque usado para dar ré quando acelerador e freio são pressionados juntos")]
    public float torqueMaximoDeRe = 800f;

    [Header("Configurações de Input")]
    [Tooltip("Valor mínimo do pedal (0 a 1) para considerar que ele está sendo pressionado")]
    [Range(0f, 1f)]
    public float limiarDePedalPressionado = 0.05f;

    [Tooltip("Inverte a leitura do eixo do volante, caso o carro vire para o lado errado")]
    public bool inverterEixoVolante = false;

    [Tooltip("Inverte a leitura dos pedais, caso o valor venha ao contrário (-1 solto, 1 fundo)")]
    public bool inverterEixoPedais = false;

    // Valores lidos do volante/pedais a cada frame
    private float valorVolante;      // -1 (esquerda) a 1 (direita)
    private float valorAcelerador;   // 0 (solto) a 1 (fundo)
    private float valorFreio;        // 0 (solto) a 1 (fundo)

    void Update()
    {
        LerInputsDoVolante();
    }

    void FixedUpdate()
    {
        AplicarDirecao();
        AplicarAceleracaoEFreio();
        AtualizarMalhasVisuais();
    }

    /// <summary>
    /// Lê os valores brutos do volante e dos pedais a partir dos eixos configurados no Inspector.
    /// </summary>
    void LerInputsDoVolante()
    {
        // --- Volante ---
        valorVolante = Input.GetAxis(eixoVolante);
        if (inverterEixoVolante)
        {
            valorVolante = -valorVolante;
        }

        // --- Pedais ---
        // Muitos volantes enviam os pedais no intervalo -1 (solto) a 1 (fundo).
        // Aqui convertemos para o intervalo 0 (solto) a 1 (fundo), que é mais intuitivo.
        float bruteAcelerador = Input.GetAxis(eixoAcelerador);
        float bruteFreio = Input.GetAxis(eixoFreio);

        if (inverterEixoPedais)
        {
            bruteAcelerador = -bruteAcelerador;
            bruteFreio = -bruteFreio;
        }

        valorAcelerador = ConverterPedalParaIntervalo0a1(bruteAcelerador);
        valorFreio = ConverterPedalParaIntervalo0a1(bruteFreio);
    }

    /// <summary>
    /// Converte o valor bruto do pedal (-1 a 1) para o intervalo 0 a 1.
    /// Se o seu volante já envia 0 a 1 nativamente, basta ajustar aqui ou marcar "inverterEixoPedais".
    /// </summary>
    float ConverterPedalParaIntervalo0a1(float valorBruto)
    {
        // Fórmula: (valor + 1) / 2  =>  transforma -1..1 em 0..1
        float valorConvertido = (valorBruto + 1f) / 2f;
        return Mathf.Clamp01(valorConvertido);
    }

    /// <summary>
    /// Aplica o ângulo de esterço nas rodas dianteiras de acordo com o volante.
    /// </summary>
    void AplicarDirecao()
    {
        float anguloDeEsterço = valorVolante * anguloMaximoDeEsterço;

        rodaDianteiraEsquerda.steerAngle = anguloDeEsterço;
        rodaDianteiraDireita.steerAngle = anguloDeEsterço;
    }

    /// <summary>
    /// Aplica torque de motor (frente ou ré) e força de freio nas rodas,
    /// seguindo as regras:
    /// - Só acelerador pressionado -> anda para frente
    /// - Só freio pressionado -> freia
    /// - Acelerador + freio juntos -> dá ré
    /// </summary>
    void AplicarAceleracaoEFreio()
    {
        bool aceleradorPressionado = valorAcelerador > limiarDePedalPressionado;
        bool freioPressionado = valorFreio > limiarDePedalPressionado;

        // Zera torque e freio antes de decidir o que aplicar neste frame
        float torqueAAplicar = 0f;
        float freioAAplicar = 0f;

        if (aceleradorPressionado && freioPressionado)
        {
            // Os dois pedais ao mesmo tempo = dar ré
            torqueAAplicar = -torqueMaximoDeRe * valorAcelerador;
            freioAAplicar = 0f;
        }
        else if (aceleradorPressionado)
        {
            // Só acelerador = anda para frente
            torqueAAplicar = torqueMaximoDoMotor * valorAcelerador;
            freioAAplicar = 0f;
        }
        else if (freioPressionado)
        {
            // Só freio = freia o carro
            torqueAAplicar = 0f;
            freioAAplicar = forcaMaximaDeFreio * valorFreio;
        }

        // Tração traseira: aplica o torque do motor nas rodas de trás
        rodaTraseiraEsquerda.motorTorque = torqueAAplicar;
        rodaTraseiraDireita.motorTorque = torqueAAplicar;

        // Freio aplicado nas 4 rodas para uma frenagem mais eficiente
        rodaDianteiraEsquerda.brakeTorque = freioAAplicar;
        rodaDianteiraDireita.brakeTorque = freioAAplicar;
        rodaTraseiraEsquerda.brakeTorque = freioAAplicar;
        rodaTraseiraDireita.brakeTorque = freioAAplicar;
    }

    /// <summary>
    /// Atualiza a posição e rotação das malhas visuais das rodas para acompanhar os WheelColliders.
    /// Isso é apenas visual; a física é sempre controlada pelos WheelColliders.
    /// </summary>
    void AtualizarMalhasVisuais()
    {
        AtualizarMalhaDeUmaRoda(rodaDianteiraEsquerda, malhaRodaDianteiraEsquerda);
        AtualizarMalhaDeUmaRoda(rodaDianteiraDireita, malhaRodaDianteiraDireita);
        AtualizarMalhaDeUmaRoda(rodaTraseiraEsquerda, malhaRodaTraseiraEsquerda);
        AtualizarMalhaDeUmaRoda(rodaTraseiraDireita, malhaRodaTraseiraDireita);
    }

    void AtualizarMalhaDeUmaRoda(WheelCollider colisorDaRoda, Transform malhaDaRoda)
    {
        // Se a malha visual não foi configurada, apenas ignora (sem quebrar o código)
        if (malhaDaRoda == null)
        {
            return;
        }

        Vector3 posicao;
        Quaternion rotacao;
        colisorDaRoda.GetWorldPose(out posicao, out rotacao);

        malhaDaRoda.position = posicao;
        malhaDaRoda.rotation = rotacao;
    }
}
