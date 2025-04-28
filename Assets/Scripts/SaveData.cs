using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int monedas = 0;
    public EstadisticasJugador estadisticas = new EstadisticasJugador();
    public List<string> cartasCompradas = new List<string>();
    public string cartaSeleccionada = "";
    public List<string> mazoPersonalizado = new List<string>();

    public SaveData()
    {
        monedas = 0;
        estadisticas = new EstadisticasJugador();
        cartasCompradas = new List<string>();
        cartaSeleccionada = "";
        mazoPersonalizado = new List<string>();
    }
}

[Serializable]
public class EstadisticasJugador
{
    // Partidas
    public int partidasJugadas;
    public int partidasGanadas;
    public int partidasPerdidas;
    public int vecesQueTeFuiste;

    // Truco
    public int trucosCantados;
    public int trucosAceptados;
    public int retrucosCantados;
    public int retrucosAceptados;
    public int valeCuatroCantados;
    public int valeCuatroAceptados;

    // Envido
    public int envidosCantados;
    public int envidosAceptados;
    public int realEnvidosCantados;
    public int realEnvidosAceptados;
    public int faltaEnvidosCantados;
    public int faltaEnvidosAceptados;

    // Envidos Ganados
    public int envidosGanados;
    public int realEnvidosGanados;
    public int faltaEnvidosGanados;

    // Envidos Perdidos
    public int envidosPerdidos;
    public int realEnvidosPerdidos;
    public int faltaEnvidosPerdidos;

    // Truco Ganados
    public int trucosGanados;
    public int retrucosGanados;
    public int valeCuatroGanados;

    // Truco Perdidos
    public int trucosPerdidos;
    public int retrucosPerdidos;
    public int valeCuatroPerdidos;
}
