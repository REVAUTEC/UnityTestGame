namespace Autobazar.Core
{
    /// <summary>
    /// Globální herní stav. Zatím drží jen příznak, jestli běží dialog/menu –
    /// když ano, pozastaví se pohyb hráče, kamera i interakce.
    /// </summary>
    public static class GameState
    {
        public static bool InputLocked;
    }
}
