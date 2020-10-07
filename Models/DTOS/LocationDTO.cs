using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public class LocationDTO
{
    public int IdCompany { get; set; }
    public string Address { get; set; }
}

class LocationDTOComparer : IEqualityComparer<LocationDTO>
{
    public bool Equals(LocationDTO b1, LocationDTO b2)
    {
        if (b2 == null && b1 == null)
            return true;
        else if (b1 == null || b2 == null)
            return false;
        else if (b1.Address == b2.Address && b1.IdCompany == b2.IdCompany)
            return true;
        else
            return false;
    }

    public int GetHashCode(LocationDTO bx)
    {
        int hCode = (bx.IdCompany + bx.Address).GetHashCode();
        return hCode.GetHashCode();
    }
}

