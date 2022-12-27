namespace Domain;

public interface IDatabaseEntity
{
    public int Id { get; set; }
    public void Refresh(IDatabaseEntity other, bool partial = false);
}