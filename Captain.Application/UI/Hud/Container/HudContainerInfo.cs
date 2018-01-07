namespace Captain.Application {
  /// <summary>
  ///   Holds information about the HUD container
  /// </summary>
  internal struct HudContainerInfo {
    /// <summary>
    ///   Container type
    /// </summary>
    internal HudContainerType ContainerType;

    /// <summary>
    ///   Mouse hook behaviour for this container
    /// </summary>
    internal Behaviour MouseHookBehaviour;

    /// <summary>
    ///   Keyboard hook behaviour for this container
    /// </summary>
    internal Behaviour KeyboardHookBehaviour;

    /// <summary>
    ///   Tidbit manager for this container
    /// </summary>
    internal TidbitManager TidbitManager;
  }
}