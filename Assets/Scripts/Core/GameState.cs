namespace YanderesFrequency.Core
{
    public enum GameState
    {
        Intro,     // Waiting for intro to finish
        Narrative, // Player reads story, time is frozen
        Action     // Player inputs Morse code, patience drains
    }
}
