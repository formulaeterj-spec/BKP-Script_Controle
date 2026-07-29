using UnityEngine;

/// <summary>
/// Script de DEBUG para descobrir quais eixos (Axis) o volante VINIK
/// está usando no Input Manager da Unity.
/// 
/// COMO USAR:
/// 1. Crie um GameObject vazio na cena (ex: "DebugAxis")
/// 2. Arraste este script para ele
/// 3. Rode o jogo (Play)
/// 4. Abra a janela Console (Window > General > Console)
/// 5. Mexa o volante e pise nos pedais, observando quais valores mudam
/// 6. Anote os nomes dos eixos que reagem (ex: "Joy1 Axis 1")
/// 
/// IMPORTANTE: Antes de usar, configure os eixos no Input Manager:
/// Edit > Project Settings > Input Manager > Axes
/// Crie entradas do tipo "Joy1 Axis 1", "Joy1 Axis 2", "Joy1 Axis 3", etc,
/// com "Type" = Joystick Axis, "Axis" correspondente, e "Sensitivity/Dead" ajustados.
/// </summary>
public class WheelAxisDebugger : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Quantidade de eixos que serão testados/exibidos no Console")]
    public int quantidadeDeEixosParaTestar = 10;

    [Tooltip("Valor mínimo de variação para considerar que o eixo está sendo usado")]
    public float limiarDeAtividade = 0.05f;

    // Guarda o último valor lido de cada eixo, para só logar quando houver mudança relevante
    private float[] ultimosValores;

    void Start()
    {
        ultimosValores = new float[quantidadeDeEixosParaTestar];

        Debug.Log("=== WheelAxisDebugger iniciado ===");
        Debug.Log("Verifique se os eixos 'Joy1 Axis 1' até 'Joy1 Axis " +
                  quantidadeDeEixosParaTestar + "' estão criados no Input Manager.");

        // Lista os dispositivos joystick/volante conectados
        string[] dispositivosConectados = Input.GetJoystickNames();
        Debug.Log("Dispositivos detectados: " + dispositivosConectados.Length);
        for (int i = 0; i < dispositivosConectados.Length; i++)
        {
            Debug.Log("Dispositivo " + i + ": " + dispositivosConectados[i]);
        }
    }

    void Update()
    {
        // Percorre os eixos "Joy1 Axis 1" até "Joy1 Axis N" e mostra no Console
        // apenas quando o valor mudar de forma perceptível (evita spam no log)
        for (int i = 1; i <= quantidadeDeEixosParaTestar; i++)
        {
            string nomeDoEixo = "Joy1 Axis " + i;
            float valorAtual = 0f;

            // GetAxisRaw pode lançar erro se o eixo não existir no Input Manager,
            // então protegemos com try/catch
            try
            {
                valorAtual = Input.GetAxisRaw(nomeDoEixo);
            }
            catch
            {
                continue; // Eixo não configurado no Input Manager, pula para o próximo
            }

            float diferenca = Mathf.Abs(valorAtual - ultimosValores[i - 1]);

            if (diferenca > limiarDeAtividade)
            {
                Debug.Log(nomeDoEixo + " = " + valorAtual.ToString("F3"));
                ultimosValores[i - 1] = valorAtual;
            }
        }

        // Também mostra todas as teclas/botões de joystick pressionadas (útil para
        // pedais que às vezes aparecem como botão em vez de eixo)
        for (int botao = 0; botao <= 19; botao++)
        {
            string nomeDoBotao = "joystick 1 button " + botao;
            if (Input.GetKeyDown(nomeDoBotao))
            {
                Debug.Log("Botão pressionado: " + nomeDoBotao);
            }
        }
    }
}
