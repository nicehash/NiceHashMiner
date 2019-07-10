namespace NHM.Common.Enums
{
    public enum NhmConectionType
    {
        NONE,
        STRATUM_TCP,
        STRATUM_SSL,
        LOCKED, // inhouse miners that are locked on NH (our eqm)
        SSL
    }
}
