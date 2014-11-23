// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A Panorama control whose background fades into the new background when
    /// set to a new value.
    /// </summary>
    public class DynamicBackgroundPanorama : Panorama
    {
        /// <summary>
        /// Initializes a new instance of the DynamicBackgroundPanorama type.
        /// </summary>
        public DynamicBackgroundPanorama() : base()
        {
            DefaultStyleKey = typeof(DynamicBackgroundPanorama);
        }
    }
}
