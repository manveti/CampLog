using System;
using System.Collections.Generic;
using System.Windows;

namespace CampLog {
    public static class CharacterSheetSpecs {
        public static readonly SortedDictionary<string, CharacterSheetFactory> specs = new SortedDictionary<string, CharacterSheetFactory>() {
            ["None"] = new CharacterSheetFactory(),
        };
    }


    [Serializable]
    public class CharacterSheet {
        public virtual Window character_window(CampaignState state, Guid? guid = null) => new SimpleCharacterWindow(state, guid);
    }


    public class CharacterSheetFactory {
        public virtual CharacterSheet get_character_sheet() => new CharacterSheet();
    }


    public interface ICharacterWindow {
        public List<EntryAction> get_actions();
    }
}