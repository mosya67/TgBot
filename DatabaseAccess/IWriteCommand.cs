namespace Domain
{
    public interface IWriteCommand<TIn>
    {
        public void Write(TIn parameter);
    }
}
