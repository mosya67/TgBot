namespace Domain
{
    public interface IWriteCommand<TIn>
    {
        public void Write(TIn parameter);
    }

    public interface IWriteCommand<TOut, TIn>
    {
        public TOut Write(TIn parameter);
    }
}
