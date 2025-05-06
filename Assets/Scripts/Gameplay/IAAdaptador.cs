using System.Collections.Generic;
using UnityEngine;

public static class IAAdaptador
{
    public static void ModificarChanceTruco(ref float valor, float delta, EstiloIA estilo)
    {
        valor = AplicarConLimite(valor, delta, estilo, campo: "Truco");
    }

    public static void ModificarChanceResponderTruco(ref float valor, float delta, EstiloIA estilo)
    {
        valor = AplicarConLimite(valor, delta, estilo, campo: "ResponderTruco");
    }

    public static void ModificarChanceIrse(ref float valor, float delta, EstiloIA estilo)
    {
        valor = AplicarConLimite(valor, delta, estilo, campo: "Irse");
    }

    public static void ModificarChanceEnvido(ref float valor, float delta, EstiloIA estilo)
    {
        valor = AplicarConLimite(valor, delta, estilo, campo: "Envido");
    }

    public static void ModificarChanceResponderEnvido(ref float valor, float delta, EstiloIA estilo)
    {
        valor = AplicarConLimite(valor, delta, estilo, campo: "ResponderEnvido");
    }

    private static float AplicarConLimite(float actual, float delta, EstiloIA estilo, string campo)
    {
        if (estilo == EstiloIA.Adaptativo)
            return Mathf.Clamp01(actual + delta);

        if (!LimitesIA.tabla.TryGetValue(estilo, out var limites))
            return Mathf.Clamp01(actual + delta); // fallback sin límite si no hay entrada

        return campo switch
        {
            "Truco" => Mathf.Clamp(actual + delta, limites.minTruco, limites.maxTruco),
            "ResponderTruco" => Mathf.Clamp(actual + delta, limites.minResponderTruco, limites.maxResponderTruco),
            "Irse" => Mathf.Clamp(actual + delta, limites.minIrse, limites.maxIrse),
            "Envido" => Mathf.Clamp(actual + delta, limites.minEnvido, limites.maxEnvido),
            "ResponderEnvido" => Mathf.Clamp(actual + delta, limites.minResponderEnvido, limites.maxResponderEnvido),
            _ => Mathf.Clamp01(actual + delta)
        };
    }
}
