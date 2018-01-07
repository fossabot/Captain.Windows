namespace Captain.Application {
  /// <summary>
  ///   Defines global behaviours on which multiple components may depend.
  /// </summary>
  internal abstract class Behaviour {
    /// <summary>
    ///   Times this behaviour was locked
    /// </summary>
    private uint lockCount;

    /// <summary>
    ///   Locks the behaviour
    /// </summary>
    protected abstract void Lock();

    /// <summary>
    ///   Unlocks the behaviour
    /// </summary>
    protected abstract void Unlock();

    /// <summary>
    ///   Requests this behaviour to be locked
    /// </summary>
    internal void RequestLock() {
      if (this.lockCount++ == 0) { Lock(); }
    }

    /// <summary>
    ///   Requests this behaviour to be unlocked
    /// </summary>
    internal void RequestUnlock() {
      if (this.lockCount > 0 && --this.lockCount == 0) { Unlock(); }
    }
  }
}