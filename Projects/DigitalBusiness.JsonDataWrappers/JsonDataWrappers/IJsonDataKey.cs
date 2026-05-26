using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers
{
    /// <summary>
    /// Marker interface for types used as keys to identify the shape of a <see cref="JsonData{T}"/>.
    /// <para>
    /// <c>T</c> is a phantom type — it carries no runtime data. Its sole purpose is to give the compiler
    /// a type token so that extension methods can be scoped to a specific JSON structure.
    /// </para>
    /// <para>
    /// Key types can inherit from other key types to layer extension methods:
    /// extensions defined for a base key are also available on derived keys.
    /// </para>
    /// <example>
    /// <code>
    /// public class ArticleItem : IJsonDataKey { }
    /// public class FeaturedArticleItem : ArticleItem { }  // inherits ArticleItem extensions
    /// </code>
    /// </example>
    /// </summary>
    public interface IJsonDataKey
    {
    }
}
