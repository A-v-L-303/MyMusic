namespace MyMusic.Application.Common.Exceptions;

public sealed class DiscogsUnavailableException()
    : Exception("Die Discogs-API ist aktuell nicht erreichbar oder liefert einen Fehler.");
