using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class Country : BaseEntity
{
    public string Name { get; set; }
    public string TwoLetterIsoCode { get; set; }
    public string ThreeLetterIsoCode { get; set; }
    public int NumericIsoCode { get; set; }
    public bool Active { get; set; }
    public int DisplayOrder { get; set; }
    public string CountryCode { get; set; }
    public string PhoneNumberPattern { get; set; }
    public int PhoneNumberMaxLength { get; set; }
    public string PhoneNumberPlaceHolder { get; set; }
    public string ZipCodePattern { get; set; }
    public string ZipCodeLabel { get; set; }
    public int ZipCodeMaxLength { get; set; }
    public string ZipCodePlaceHolder { get; set; }

    public virtual ICollection<StateProvince> StateProvinces { get; set; } = new List<StateProvince>();
}

public class CustomCountry
{
    public string CountryCode { get; set; }
    public string PhoneNumberPattern { get; set; }
    public int PhoneNumberMaxLength { get; set; }
    public string PhoneNumberPlaceHolder { get; set; }
    public string ZipCodePattern { get; set; }
    public string ZipCodeLabel { get; set; }
    public int ZipCodeMaxLength { get; set; }
    public string ZipCodePlaceHolder { get; set; }
}