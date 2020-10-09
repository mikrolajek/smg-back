using System.Collections.Generic;

public class GroupDTO

{
    public int Location { get; set; }
    public int Link { get; set; }
    public int Product { get; set; }
}

class GroupDTOComparer : IEqualityComparer<GroupDTO>
{
    public bool Equals(GroupDTO group1, GroupDTO group2)
    {
        if (group2 == null && group1 == null)
            return true;
        else if (group1 == null || group2 == null)
            return false;
        else if (group1.Product == group2.Product && group1.Location == group2.Location && group1.Link == group2.Link)
            return true;
        else
            return false;
    }

    public int GetHashCode(GroupDTO group)
    {
        int hCode = (group.Link + group.Location + group.Product).GetHashCode();
        return hCode.GetHashCode();
    }
}

