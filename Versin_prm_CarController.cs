using UnityEngine;

public class CarController : MonoBehaviour
{
    // Configurações do carro
    [Header("Velocidade")]
    [SerializeField] private float velocidadeMaxima = 65f;
    [SerializeField] private float velocidadeMaximaRe = 28f;

    [Header("Direção")]
    [SerializeField] private float anguloMaximoDirecao = 37f;

    private float velocidadeAtual = 0f;
    private float anguloDirecao = 0f;   // Ângulo atual das rodas
    private float anguloAlvo = 0f;      // Direção desejada pelo jogador

    private float entradaAceleracao = 0f;
    private float entradaDirecao = 0f;

    private readonly Vector3 direcaoFrente = Vector3.zero;

    // Referência ao transform do carro
    public Transform carroTransform;

    //================ Método Principal ==================================
    private void Update()
    {
        Atualizar(Time.deltaTime);
    }

    public void Atualizar(float delta)
    {
        if (carroTransform == null)
        {
            Debug.LogError("[ControladorCarro] Transform não foi definido!");
            return;
        }

        AtualizarInput(delta);
        AtualizarFisica(delta);
        AplicarMovimento(delta);
    }

    private void AtualizarInput(float delta)
    {
        entradaDirecao = 0f; // -1 = esquerda | +1 = direita
        entradaAceleracao = 0f;

        // Verifica se há algum controle (joystick) conectado
        string[] joysticks = Input.GetJoystickNames();
        bool temControle = joysticks.Length > 0 && !string.IsNullOrEmpty(joysticks[0]);

        if (temControle)
        {
            entradaDirecao = Input.GetAxis("Horizontal"); // eixo do analógico esquerdo
            float rt = Input.GetAxis("RT"); // configure esses eixos no Input Manager
            float lt = Input.GetAxis("LT");

            entradaAceleracao = rt - lt;
        }
        else
        {
            // Teclado
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) entradaDirecao -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) entradaDirecao += 1;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) entradaAceleracao = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) entradaAceleracao = -0.75f;
        }

        anguloAlvo = entradaDirecao * anguloMaximoDirecao;
        anguloDirecao = Mathf.Lerp(anguloDirecao, anguloAlvo, 10f * delta);
    }

    private void AtualizarFisica(float delta)
    {
        if (entradaAceleracao > 0.1f)
        {
            velocidadeAtual += entradaAceleracao * 55f * delta;
        }
        else if (entradaAceleracao < -0.1f)
        {
            velocidadeAtual += entradaAceleracao * 65f * delta;
        }
        else
        {
            velocidadeAtual *= 0.935f;
        }

        velocidadeAtual = Mathf.Clamp(velocidadeAtual, -velocidadeMaximaRe, velocidadeMaxima);
    }

    private void AplicarMovimento(float delta)
    {
        float yaw = carroTransform.eulerAngles.y;

        Vector3 direcao = Quaternion.Euler(0, yaw, 0) * Vector3.back;

        float distancia = velocidadeAtual * delta * 14f;
        Vector3 deslocamento = direcao * distancia;

        carroTransform.position += deslocamento;

        if (Mathf.Abs(velocidadeAtual) > 3f)
        {
            float fatorRe = velocidadeAtual > 0 ? 1f : -0.5f;
            carroTransform.Rotate(Vector3.up, anguloDirecao * fatorRe * delta * 4.8f);
        }
    }

    // Getters
    public float GetAnguloDirecao() => anguloDirecao;
    public float GetVelocidadeAtual() => velocidadeAtual;
}