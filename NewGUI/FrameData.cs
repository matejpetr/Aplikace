using System.Collections.Generic;

namespace NewGUI
{

    // Tøída FrameData pøedstavuje jeden "datovı rámec" pøijatı ze senzoru/aktuátoru.
    // Kadı rámec obsahuje index (poøadí), sadu hodnot (napø. mìøené velièiny)
    // a textovou reprezentaci tìchto hodnot (napø. pro zobrazení v UI).
    public class FrameData
    {
        // Pomáhá urèit, v jakém poøadí rámce dorazily.
        public int Index { get; }
        // IReadOnlyDictionary znamená, e zvenku se do nìj u nedá zapisovat (jen èíst).
        public IReadOnlyDictionary<string, double> Values { get; }
        // Hodí se tøeba pro rychlé vypsání na obrazovku nebo do logu.
        public string ValueText { get; }

        //Konstruktor: vytvoøí novı datovı rámec z dodanıch údajù.
        public FrameData(int index, IDictionary<string, double> values, string valueText)
        {
            Index = index; // Uloí poøadí rámce
            // Zkopíruje hodnoty do nového Dictionary, aby pùvodní data nešla zvenèí zmìnit (zajišuje imutabilitu objektu). Pokud values == null, vytvoøí prázdnı slovník.
            Values = values == null ? new Dictionary<string,double>() : new Dictionary<string,double>(values);
            ValueText = valueText ?? string.Empty; // Uloí textovou reprezentaci; pokud je null, nahradí ji prázdnım øetìzcem.
        }
    }
}
