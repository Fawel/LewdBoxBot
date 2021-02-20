using System;

namespace LBox.Shared
{
    public struct IntNaturalRange
    {
        private readonly int _minValue;
        private readonly int _maxValue;

        public IntNaturalRange(int minValue, int maxValue)
        {
            if (minValue < 0 || maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("Числа диапазона должны быть больше либо равны 0");
            }

            if (maxValue != 0 && minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("Минимальное число диапазона больше максимального");
            }

            _minValue = minValue;
            _maxValue = maxValue;
        }

        public bool IsEmptyRange() => _minValue == 0 && _maxValue == 0;

        public static IntNaturalRange CreateRangeWithMinimalLimit(int minValue) =>
            new IntNaturalRange(minValue, 0);
        public static IntNaturalRange CreateRangeWithMaximumLimit(int maxValue) =>
            new IntNaturalRange(0, maxValue);

        public static IntNaturalRange CreateEmptyRange() =>
            new IntNaturalRange(0, 0);

        public bool CheckInRange(int? valueToCheck)
        {
            if (IsEmptyRange())
            {
                return true;
            }
            
            if(!valueToCheck.HasValue)
            {
                return false;
            }

            return _minValue < valueToCheck && (_maxValue == 0 || _maxValue > valueToCheck);
        }
    }
}
