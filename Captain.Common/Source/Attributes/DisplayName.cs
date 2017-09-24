﻿using System;

namespace Captain.Common {
  /// <summary>
  ///   Specifies a custom display name for this object
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public class DisplayName : Attribute {
    /// <summary>
    ///   Language code (may be <c>null</c>)
    /// </summary>
    public string LanguageCode { get; }

    /// <summary>
    ///   Display name
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///   Sets the neutral display name for this object
    /// </summary>
    /// <param name="name">Display name</param>
    public DisplayName(string name) => Name = name;

    /// <summary>
    ///   Sets the localized display name for this object
    /// </summary>
    /// <param name="languageCode">ISO language code (i.e. en-GB)</param>
    /// <param name="name">Display name</param>
    public DisplayName(string languageCode, string name) => (LanguageCode, Name) = (languageCode, name);
  }
}