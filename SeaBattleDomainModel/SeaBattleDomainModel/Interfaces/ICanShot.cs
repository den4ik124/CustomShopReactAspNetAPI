namespace SeaBattleDomainModel.Interfaces
{
    internal interface ICanShot
    {
        /// <summary>
        /// Производит выстрел в указанную точку
        /// </summary>
        /// <param name="target">Точка, в которую будет произведен выстрел</param>
        void MakeShot();
    }
}