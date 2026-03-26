using System.ComponentModel.DataAnnotations;


namespace Omnimarket.Api.Models.Enum

{
    public enum TiposLogradouroBR
    {

        Nenhum = 0,
        
        [Display(Name = "Rua (R)")]
        R,

        [Display(Name = "Avenida (AV)")]
        AV,

        [Display(Name = "Travessa (TV)")]
        TV,

        [Display(Name = "Alameda (AL)")]
        AL,

        [Display(Name = "Praça (PC)")]
        PC,

        [Display(Name = "Estrada (EST)")]
        EST,

        [Display(Name = "Rodovia (ROD)")]
        ROD,

        [Display(Name = "Viela (VLA)")]
        VLA,

        [Display(Name = "Vila (VL)")]
        VL,

        [Display(Name = "Largo (LRG)")]
        LRG,

        [Display(Name = "Ladeira (LD)")]
        LD,

        [Display(Name = "Conjunto (CJ)")]
        CJ,

        [Display(Name = "Quadra (Q)")]
        Q,

        [Display(Name = "Beco (BC)")]
        BC
    }
}