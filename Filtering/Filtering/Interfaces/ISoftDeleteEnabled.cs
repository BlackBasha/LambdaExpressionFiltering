namespace DALForInsurance.Interfaces
{
    public interface ISoftDeleteEnabled
    {
        bool IsDeleted { get; set; }
    }
}
