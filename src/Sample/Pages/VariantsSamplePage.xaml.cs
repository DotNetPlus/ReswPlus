// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace ReswPlusSample.Pages
{
    public enum PartDayEnum { MORNING = 1, AFTERNOON, EVENING, NIGHT };
    public enum PetTypeEnum { DOG= 1, CAT };

    public sealed partial class VariantsSamplePage : Page, INotifyPropertyChanged
    {
        public VariantsSamplePage()
        {
            this.InitializeComponent();
        }

        public PartDayEnum DayPart { get; set; }

        private void UpdatePartDay(PartDayEnum part)
        {
            DayPart = part;
            RaisePropertyChanged(nameof(DayPart));
        }

        public PetTypeEnum PetType{ get; set; }

        private void UpdatePetType(PetTypeEnum type)
        {
            PetType = type;
            RaisePropertyChanged(nameof(PetType));
        }

        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void PartDayMorning_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UpdatePartDay(PartDayEnum.MORNING);
        }

        private void PartDayAfternoon_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UpdatePartDay(PartDayEnum.AFTERNOON);
        }

        private void PartDayEvening_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UpdatePartDay(PartDayEnum.EVENING);
        }

        private void PartDayNight_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UpdatePartDay(PartDayEnum.NIGHT);
        }

        private void PetTypeDog_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UpdatePetType(PetTypeEnum.DOG);
        }

        private void PetTypeCat_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UpdatePetType(PetTypeEnum.CAT);
        }
    }
}
