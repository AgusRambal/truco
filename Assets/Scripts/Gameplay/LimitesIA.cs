using System.Collections.Generic;

public class LimitesIA
{
    public float minTruco, maxTruco;
    public float minRetruco, maxRetruco;
    public float minValeCuatro, maxValeCuatro;
    public float minResponderTruco, maxResponderTruco;
    public float minEnvido, maxEnvido;
    public float minResponderEnvido, maxResponderEnvido;
    public float minIrse, maxIrse;

    public static readonly Dictionary<EstiloIA, LimitesIA> tabla = new()
    {
        {
            EstiloIA.Mentiroso, new LimitesIA
            {
                minTruco = 0.5f, maxTruco = 0.9f,
                minRetruco = 0.3f, maxRetruco = 0.7f,
                minValeCuatro = 0.1f, maxValeCuatro = 0.6f,
                minResponderTruco = 0.4f, maxResponderTruco = 0.8f,
                minEnvido = 0.3f, maxEnvido = 0.7f,
                minResponderEnvido = 0.2f, maxResponderEnvido = 0.6f,
                minIrse = 0.05f, maxIrse = 0.25f
            }
        },
        {
            EstiloIA.Conservador, new LimitesIA
            {
                minTruco = 0.1f, maxTruco = 0.4f,
                minRetruco = 0.05f, maxRetruco = 0.2f,
                minValeCuatro = 0f, maxValeCuatro = 0.1f,
                minResponderTruco = 0.5f, maxResponderTruco = 0.8f,
                minEnvido = 0.2f, maxEnvido = 0.5f,
                minResponderEnvido = 0.1f, maxResponderEnvido = 0.4f,
                minIrse = 0.3f, maxIrse = 0.6f
            }
        },
        {
            EstiloIA.Agresivo, new LimitesIA
            {
                minTruco = 0.6f, maxTruco = 1f,
                minRetruco = 0.4f, maxRetruco = 0.9f,
                minValeCuatro = 0.2f, maxValeCuatro = 0.7f,
                minResponderTruco = 0.7f, maxResponderTruco = 1f,
                minEnvido = 0.4f, maxEnvido = 0.9f,
                minResponderEnvido = 0.3f, maxResponderEnvido = 0.8f,
                minIrse = 0f, maxIrse = 0.2f
            }
        },
        {
            EstiloIA.Canchero, new LimitesIA
            {
                minTruco = 0.4f, maxTruco = 0.8f,
                minRetruco = 0.3f, maxRetruco = 0.6f,
                minValeCuatro = 0.1f, maxValeCuatro = 0.5f,
                minResponderTruco = 0.5f, maxResponderTruco = 0.85f,
                minEnvido = 0.3f, maxEnvido = 0.7f,
                minResponderEnvido = 0.3f, maxResponderEnvido = 0.7f,
                minIrse = 0.1f, maxIrse = 0.4f
            }
        },
        {
            EstiloIA.Calculador, new LimitesIA
            {
                minTruco = 0.3f, maxTruco = 0.6f,
                minRetruco = 0.2f, maxRetruco = 0.5f,
                minValeCuatro = 0.05f, maxValeCuatro = 0.3f,
                minResponderTruco = 0.6f, maxResponderTruco = 0.95f,
                minEnvido = 0.3f, maxEnvido = 0.6f,
                minResponderEnvido = 0.3f, maxResponderEnvido = 0.7f,
                minIrse = 0.15f, maxIrse = 0.35f
            }
        },
        {
            EstiloIA.Caotico, new LimitesIA
            {
                minTruco = 0.2f, maxTruco = 1f,
                minRetruco = 0.1f, maxRetruco = 1f,
                minValeCuatro = 0f, maxValeCuatro = 1f,
                minResponderTruco = 0.2f, maxResponderTruco = 1f,
                minEnvido = 0.1f, maxEnvido = 1f,
                minResponderEnvido = 0.1f, maxResponderEnvido = 1f,
                minIrse = 0f, maxIrse = 1f
            }
        }
    };
}
