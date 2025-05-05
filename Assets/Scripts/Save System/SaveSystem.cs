// SaveSystem.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SaveSystem
{
    public static SaveData Datos { get; private set; }

    public static void CargarDatos()
    {
        Datos = SaveManager.Cargar();

        if (Datos == null)
        {
            Datos = new SaveData();
        }

        if (Datos.personalidadesDefault == null || Datos.personalidadesDefault.Count == 0)
        {
            Datos.personalidadesDefault = CrearPersonalidadesDefault();
            Datos.personalidadesAdaptadas = Datos.personalidadesDefault.Select(p => new ConfiguracionIA(p)).ToList();
            GuardarDatos(); // guardamos la primera vez
        }

        if (Datos.estadisticas == null)
            Datos.estadisticas = new EstadisticasJugador();
        if (Datos.cartasCompradas == null)
            Datos.cartasCompradas = new List<string>();
        if (Datos.mazoPersonalizado == null)
            Datos.mazoPersonalizado = new List<string>();
    }

    public static void GuardarDatos()
    {
        if (Datos == null)
        {
            Debug.LogWarning("SaveSystem: Datos está null. Se crea uno nuevo antes de guardar.");
            Datos = new SaveData();
        }

        // Revalidar campos críticos
        if (Datos.estadisticas == null)
            Datos.estadisticas = new EstadisticasJugador();
        if (Datos.cartasCompradas == null)
            Datos.cartasCompradas = new List<string>();
        if (Datos.mazoPersonalizado == null)
            Datos.mazoPersonalizado = new List<string>();

        // Serializar para validar antes de encriptar
        string json = JsonUtility.ToJson(Datos, prettyPrint: true);
        if (string.IsNullOrEmpty(json) || json.Length < 10)
        {
            Debug.LogError("SaveSystem: JSON generado inválido. No se guarda.");
            return;
        }

        SaveManager.Guardar(Datos);
    }

    private static List<ConfiguracionIA> CrearPersonalidadesDefault()
    {
        return new List<ConfiguracionIA>
        {
            new ConfiguracionIA
            {
                estilo = EstiloIA.Conservador,
                chanceCantarEnvido = 0.2f,
                chanceDeQueSeaReal = 0.9f,
                chanceResponderConSubida = 0.1f,
                chanceCantarTruco = 0.2f,
                chanceCantarRetruco = 0.1f,
                chanceCantarValeCuatro = 0f,
                chanceResponderTruco = 0.8f,
                chanceResponderEnvidoTrasTruco = 0.1f,
                chanceDeIrse = 0.7f
            },
            new ConfiguracionIA
            {
                estilo = EstiloIA.Canchero,
                chanceCantarEnvido = 0.4f,
                chanceDeQueSeaReal = 0.6f,
                chanceResponderConSubida = 0.4f,
                chanceCantarTruco = 0.6f,
                chanceCantarRetruco = 0.5f,
                chanceCantarValeCuatro = 0.3f,
                chanceResponderTruco = 0.7f,
                chanceResponderEnvidoTrasTruco = 0.5f,
                chanceDeIrse = 0.2f
            },
            new ConfiguracionIA
            {
                estilo = EstiloIA.Mentiroso,
                chanceCantarEnvido = 0.5f,
                chanceDeQueSeaReal = 0.3f,
                chanceResponderConSubida = 0.5f,
                chanceCantarTruco = 0.7f,
                chanceCantarRetruco = 0.5f,
                chanceCantarValeCuatro = 0.2f,
                chanceResponderTruco = 0.5f,
                chanceResponderEnvidoTrasTruco = 0.6f,
                chanceDeIrse = 0.1f
            },
            new ConfiguracionIA
            {
                estilo = EstiloIA.Agresivo,
                chanceCantarEnvido = 0.6f,
                chanceDeQueSeaReal = 0.6f,
                chanceResponderConSubida = 0.6f,
                chanceCantarTruco = 0.8f,
                chanceCantarRetruco = 0.7f,
                chanceCantarValeCuatro = 0.5f,
                chanceResponderTruco = 0.9f,
                chanceResponderEnvidoTrasTruco = 0.7f,
                chanceDeIrse = 0.05f
            },
            new ConfiguracionIA
            {
                estilo = EstiloIA.Calculador,
                chanceCantarEnvido = 0.3f,
                chanceDeQueSeaReal = 0.95f,
                chanceResponderConSubida = 0.2f,
                chanceCantarTruco = 0.4f,
                chanceCantarRetruco = 0.3f,
                chanceCantarValeCuatro = 0.1f,
                chanceResponderTruco = 0.95f,
                chanceResponderEnvidoTrasTruco = 0.2f,
                chanceDeIrse = 0.4f
            },
            new ConfiguracionIA
            {
                estilo = EstiloIA.Caotico,
                chanceCantarEnvido = 0.5f,
                chanceDeQueSeaReal = 0.5f,
                chanceResponderConSubida = 0.5f,
                chanceCantarTruco = 0.5f,
                chanceCantarRetruco = 0.5f,
                chanceCantarValeCuatro = 0.5f,
                chanceResponderTruco = 0.5f,
                chanceResponderEnvidoTrasTruco = 0.5f,
                chanceDeIrse = 0.5f
            },
            new ConfiguracionIA
            {
                estilo = EstiloIA.Adaptativo,
                chanceCantarEnvido = 0.4f,
                chanceDeQueSeaReal = 0.8f,
                chanceResponderConSubida = 0.4f,
                chanceCantarTruco = 0.5f,
                chanceCantarRetruco = 0.5f,
                chanceCantarValeCuatro = 0.3f,
                chanceResponderTruco = 0.7f,
                chanceResponderEnvidoTrasTruco = 0.4f,
                chanceDeIrse = 0.3f
            }
        };
    }
}
