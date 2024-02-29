namespace Domain
{
    public interface IGetCommand<TOut, TIn>
    {
        public TOut Get(TIn i);
    }

    public interface IGetCommand<TOut>
    {
        public TOut Get();
    }
}
