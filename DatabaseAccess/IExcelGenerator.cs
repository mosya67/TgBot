namespace Domain
{
    public interface IExcelGenerator<TOut, TIn>
    {
        public TOut WriteResultsAsync(TIn parameter);
    }
}
