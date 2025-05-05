[System.Serializable]
public class ConfiguracionIA
{
    public EstiloIA estilo;
    public float chanceCantarEnvido;
    public float chanceDeQueSeaReal;
    public float chanceResponderConSubida;

    public float chanceCantarTruco;
    public float chanceCantarRetruco;
    public float chanceCantarValeCuatro;
    public float chanceResponderTruco;
    public float chanceResponderEnvidoTrasTruco;

    public float chanceDeIrse;

    // Constructor para clonar
    public ConfiguracionIA(ConfiguracionIA original)
    {
        estilo = original.estilo;
        chanceCantarEnvido = original.chanceCantarEnvido;
        chanceDeQueSeaReal = original.chanceDeQueSeaReal;
        chanceResponderConSubida = original.chanceResponderConSubida;

        chanceCantarTruco = original.chanceCantarTruco;
        chanceCantarRetruco = original.chanceCantarRetruco;
        chanceCantarValeCuatro = original.chanceCantarValeCuatro;
        chanceResponderTruco = original.chanceResponderTruco;
        chanceResponderEnvidoTrasTruco = original.chanceResponderEnvidoTrasTruco;

        chanceDeIrse = original.chanceDeIrse;
    }

    public ConfiguracionIA() { }
}
