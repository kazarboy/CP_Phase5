//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EPP.CorporatePortal.DAL.EDMX
{
    using System;
    using System.Collections.Generic;
    
    public partial class FileUploadFallout_remove
    {
        public int Id { get; set; }
        public Nullable<int> FileUploadId { get; set; }
        public string Fallout { get; set; }
    
        public virtual FileUpload FileUpload { get; set; }
    }
}