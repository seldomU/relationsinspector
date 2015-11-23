using System;

namespace RelationsInspector.Backend.AutoBackend
{
    // indicates that RI should make an RIAutoBackend type for the marked type
    [AttributeUsage( AttributeTargets.Class )]
    public class AutoBackendAttribute : Attribute { }

    // indicates that there's a relation from the object to each of the field's objects
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
    public class RelatedAttribute : Attribute { }

    // indicates that there's a relation from each of the field's objects to the object
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
    public class RelatingAttribute : Attribute { }
}
