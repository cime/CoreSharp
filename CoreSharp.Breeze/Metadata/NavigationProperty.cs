using System.Collections.Generic;
using CoreSharp.Breeze.Extensions;

namespace CoreSharp.Breeze.Metadata
{
    /// <summary>
    /// A single navigation property, at a minimum you must to define the 'required' properties defined below AS WELL AS either a 'name' or a 'nameOnServer'..
    /// </summary>
    public class NavigationProperty : BaseProperty
    {
        public NavigationProperty()
        {
        }

        public NavigationProperty(Dictionary<string, object> dict)
            : base(dict)
        {
        }

        #region EntityTypeName

        /// <summary>
        /// The type of the entity or collection of entities returned by this property.
        /// </summary>
        public string EntityTypeName
        {
            get { return OriginalDictionary.GetValue<string>("entityTypeName"); }
            set { OriginalDictionary["entityTypeName"] = value; }
        }

        #endregion

        #region IsScalar

        /// <summary>
        /// Whether this property returns a single entity (true) or an array of entities (false).
        /// </summary>
        public bool IsScalar
        {
            get { return OriginalDictionary.GetValue<bool>("isScalar"); }
            set { OriginalDictionary["isScalar"] = value; }
        }

        #endregion

        #region AssociationName

        /// <summary>
        /// An arbitrary name that is used to link this navigation property to its inverse property. For bidirectional navigations this name will occur twice within this document, otherwise only once.
        /// </summary>
        public string AssociationName
        {
            get { return OriginalDictionary.GetValue<string>("associationName"); }
            set { OriginalDictionary["associationName"] = value; }
        }

        #endregion

        #region ForeignKeyNames

        /// <summary>
        /// An array of the names of the properties on this type that are the foreign key 'backing' for this navigation property.  This may only be set if 'isScalar' is true.
        /// </summary>
        public IList<string> ForeignKeyNames
        {
            get { return OriginalDictionary.GetValue<IList<string>>("foreignKeyNames"); }
            set { OriginalDictionary["foreignKeyNames"] = value; }
        }

        #endregion

        #region InvForeignKeyNames

        public IList<string> InvForeignKeyNames
        {
            get { return OriginalDictionary.GetValue<IList<string>>("invForeignKeyNames"); }
            set { OriginalDictionary["invForeignKeyNames"] = value; }
        }

        #endregion

        #region ForeignKeyNamesOnServer

        /// <summary>
        /// Same as ForeignKeyNames, but the names here are server side names as opposed to client side.  Only one or the other is needed.
        /// </summary>
        public IList<string> ForeignKeyNamesOnServer
        {
            get { return OriginalDictionary.GetValue<IList<string>>("foreignKeyNamesOnServer"); }
            set { OriginalDictionary["foreignKeyNamesOnServer"] = value; }
        }

        #endregion

        #region InvForeignKeyNamesOnServer

        public IList<string> InvForeignKeyNamesOnServer
        {
            get { return OriginalDictionary.GetValue<IList<string>>("invForeignKeyNamesOnServer"); }
            set { OriginalDictionary["invForeignKeyNamesOnServer"] = value; }
        }

        #endregion

        #region Validators

        private Validators _validators;

        /// <summary>
        /// A list of the validators (validations) that will be associated with this property
        /// </summary>
        public Validators Validators
        {
            get
            {
                if (_validators != null)
                    return _validators;
                if (!OriginalDictionary.ContainsKey("validators"))
                    OriginalDictionary["validators"] = new List<Dictionary<string, object>>();
                _validators = new Validators(OriginalDictionary["validators"] as List<Dictionary<string, object>>);
                return _validators;
            }
            set
            {
                _validators = value;
                OriginalDictionary["validators"] = value.OriginalList;
            }
        }

        #endregion
    }
}
