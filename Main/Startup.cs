namespace TownRPG.Main {
    public class Startup {

        public static void Main() {
            using (var game = new GameImpl()) {
                game.Run();
            }
        }

    }
}