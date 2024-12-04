using Idibri.RevitPlugin.Common.Infrastructure;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Models
{
    public class CableBundleMember : ModelBase
    {
        #region Properties
        public Cable Cable { get; private set; }
        public int Quantity { get; private set; }
        public double? AreaIn { get; private set; }
        #endregion

        #region Constructors
        public CableBundleMember(Cable cable, int quantity)
            : base()
        {
            Cable = cable;
            Quantity = quantity;
            AreaIn = cable.AreaIn.HasValue ? cable.AreaIn.Value * quantity : (double?)null;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return string.Format("{0}{1}", Quantity, Cable.Name);
        }
        #endregion
    }
}