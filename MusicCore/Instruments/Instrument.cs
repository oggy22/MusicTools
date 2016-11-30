namespace MusicComposer.Instruments
{
    abstract class Instrument
    {
        /// <summary>
        /// The usual number of parallel voices e.g. 1 for violin, 3-4 for piano
        /// </summary>
        public abstract int ParallelVoices { get; }

        /// <summary>
        /// The maximal number of parallel voices e.g. two for violin, 10 for piano
        /// </summary>
        public abstract int MaxParallelVoices { get; }
    }
}
