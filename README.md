# Clicker_HECS
Игра в жанре Idle/Clicker с системой экипировки и скиллов персонажа. Игроку необходимо зачищать подземелья/открытые зоны, побеждая врагов игрок получает различную экипировку (рандомная генерация по правилам) и скиллы (в дальнейшем планировалось добавить еще несколько видов слотов). Передвижение по уровню линейное и автоматическое, игрок может только кликать/тапать ускоряя прохождение подземелья. Проект разрабатывался с целью выхода на MVP с дальнейшим поиском финансирования. Разработкой по большей части занимался я, под руководством Евгения Дубовика.

- [DrawRules](Systems/DrawRules) - системы управления частицами партикл систем для красивой отрисовки летящих монеток или мешочков из побежденного моба к счетчикам/в сундук. Из сложностей - UI находился на отдельной камере, а камера рендерящая игровой мир имела своеобразный наклон, партиклы спавнились в 3D в world, поэтому двигать их в мире было необходимо с учетом положения UI на другой камере и наклона игровой камеры.
- [AbilitiesSystems](Systems/Abilities) - системы умений (автоатака и тап)
- [EquipmentGenerator](Systems/EquipItemsGenerator) - системы генератора экипировки
- [EquipmentSystems](Systems/Equipment) - системы экипировки игрока
- [PlayerSystems](Systems/Player) - системы для управления игроком
- [GameStates](Systems/GameStates) - системы управления стейтами игры
- [UISystems](Systems/UI) - системы управления UI
- [CustomConfigs](CustomConfigs) - различные конфиги для работы игры

