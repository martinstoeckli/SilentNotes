using System.Collections.Generic;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// All necessary information to do a binding to a property of a viewmodel.
    /// </summary>
    public class BindingDescription
    {
        /// <summary>
        /// Gets or sets the name of the property to bind. This value is case sensitive.
        /// </summary>
        public string PropertyName { get; set; }

        public HtmlViewBindingMode Mode { get; set; }
    }

    /// <summary>
    /// List of <see cref="BindingDescription"/> objects.
    /// </summary>
    public class BindingDescriptions : List<BindingDescription>
    {
        /// <summary>
        /// Initilaizes a new instance of the <see cref="BindingDescriptions"/> class.
        /// </summary>
        public BindingDescriptions()
        {
        }

        /// <summary>
        /// Initilaizes a new instance of the <see cref="BindingDescriptions"/> class.
        /// </summary>
        /// <param name="descriptions">Enumeration of descriptions which are applied to the list.</param>
        public BindingDescriptions(IEnumerable<BindingDescription> descriptions)
        {
            AddRange(descriptions);
        }

        /// <summary>
        /// Searches the list for an element with this property name and returns the first found
        /// element.
        /// </summary>
        /// <param name="propertyName">Name of the property to search for.</param>
        /// <returns>First found element or null if no such element was found.</returns>
        public BindingDescription FindByPropertyName(string propertyName)
        {
            return Find(item => string.Equals(propertyName, item.PropertyName));
        }
    }
}
