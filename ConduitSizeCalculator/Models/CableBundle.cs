using System.Collections;
using System.Collections.Generic;
using Idibri.RevitPlugin.Common.Infrastructure;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Models
{
    public class CableBundle : ModelBase, IEnumerable<CableBundleMember>
    {
        #region Properties
        public IEnumerable<CableBundleMember> Cables { get { return _cables; } }
        private List<CableBundleMember> _cables;

        public bool IsEmpty { get; set; }

        public double? CableAreaIn { get; private set; }
        #endregion

        #region Constructors
        public CableBundle()
            : base()
        {
            CableAreaIn = 0;
        }
        #endregion

        #region Methods
        public void Add(CableBundleMember cableDefinition)
        {
            if (_cables == null)
            {
                _cables = new List<CableBundleMember>();
            }
            _cables.Add(cableDefinition);
            if (CableAreaIn.HasValue)
            {
                if (cableDefinition.AreaIn.HasValue)
                {
                    CableAreaIn += cableDefinition.AreaIn;
                }
                else
                {
                    CableAreaIn = null;
                }
            }
        }

        public double GetMinimumConduitArea(double maximumCableAreaPercentage)
        {
            if (!CableAreaIn.HasValue)
            {
                throw new NoSizeSpecifiedException();
            }
            return CableAreaIn.Value / maximumCableAreaPercentage;
        }

        IEnumerator<CableBundleMember> IEnumerable<CableBundleMember>.GetEnumerator() { return Cables.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return Cables.GetEnumerator(); }
        #endregion
    }
}
