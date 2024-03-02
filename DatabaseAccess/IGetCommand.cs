namespace Domain
{
    public interface IGetCommand<TOut, TIn>
    {
        public TOut Get(TIn parameter);
    }

    public interface IGetCommand<TOut>
    {
        public TOut Get();
    }
}
