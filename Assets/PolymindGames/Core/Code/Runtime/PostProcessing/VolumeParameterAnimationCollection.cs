using System.Collections.Generic;

namespace PolymindGames.PostProcessing
{
    public sealed class VolumeParameterAnimationCollection
    {
        private readonly List<VolumeParameterAnimation> _list;


        public VolumeParameterAnimationCollection()
        {
            _list = new List<VolumeParameterAnimation>(1);
        }

        public int Count => _list.Count;

        public VolumeParameterAnimation this[int index] => _list[index];

        public void Add(VolumeParameterAnimation parameter)
        {
            if (CanAdd(parameter))
                _list.Add(parameter);
        }
        
        public void Clear() => _list.Clear();
        public bool Remove(VolumeParameterAnimation item) => _list.Remove(item);

        private bool CanAdd(VolumeParameterAnimation parameter) =>
            parameter != null && parameter.Enabled && !_list.Contains(parameter);
    }
}
