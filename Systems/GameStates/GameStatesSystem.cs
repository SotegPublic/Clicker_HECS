using System;
using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Systems
{
    [Serializable][Documentation(Doc.GameLogic, Doc.GameState, "GameStatesSystem its main system for rule the game logic")]
    public sealed class GameStatesSystem : BaseMainGameLogicSystem, IReactGlobalCommand<GoToNextBiomCommand>
    {
        private bool isBiomEnd;
        private SceneVariablesComponent sceneVariables;

        public void CommandGlobalReact(GoToNextBiomCommand command)
        {
            isBiomEnd = true;
        }

        public override void GlobalStart()
        {
            sceneVariables = Owner.World.GetEntityBySingleComponent<SceneManagerTagComponent>().GetComponent<SceneVariablesComponent>();
            ChangeGameState(GameStateIdentifierMap.LoadPlayerStateIdentifier);
        }

        public override void InitSystem()
        {
        }

        protected override void ProcessEndState(EndGameStateCommand endGameStateCommand)
        {
            switch (endGameStateCommand.GameState)
            {
                case GameStateIdentifierMap.LoadPlayerStateIdentifier:
                    Owner.World.Command(new UIGroupCommand { Show = true, UIGroup = UIGroupIdentifierMap.MainScreen });
                    ChangeGameState(GameStateIdentifierMap.LoadBiomStateIdentifier);
                    break;
                case GameStateIdentifierMap.LoadBiomStateIdentifier:
                    ChangeGameState(GameStateIdentifierMap.PreloadStateIdentifier);
                    break;
                case GameStateIdentifierMap.PreloadStateIdentifier:

                    if (sceneVariables.CurrentBiomMoveType == BiomMoveTypeIdentifierMap.Chunk)
                    {
                        ChangeGameState(GameStateIdentifierMap.LoadInitialChunksStateIdentifier);
                    }

                    if (sceneVariables.CurrentBiomMoveType == BiomMoveTypeIdentifierMap.TravelPrefab || sceneVariables.CurrentBiomMoveType == BiomMoveTypeIdentifierMap.TravelScene)
                    {
                        ChangeGameState(GameStateIdentifierMap.SpawnPlayerCharactersInTravelBiomStateIdentifier);
                    }

                    break;
                case GameStateIdentifierMap.GameInProgressStateIdentifier:

                    if(sceneVariables.CurrentBiomMoveType == BiomMoveTypeIdentifierMap.Chunk)
                    {
                        ChangeGameState(GameStateIdentifierMap.ChangeChunkStateIdentifier);
                    }

                    if (sceneVariables.CurrentBiomMoveType == BiomMoveTypeIdentifierMap.TravelPrefab || sceneVariables.CurrentBiomMoveType == BiomMoveTypeIdentifierMap.TravelScene)
                    {
                        ChangeGameState(GameStateIdentifierMap.ChangeTravelPointStateIdentifier);
                    }

                    break;

                // Chunks states
                case GameStateIdentifierMap.LoadInitialChunksStateIdentifier:
                    ChangeGameState(GameStateIdentifierMap.SpawnPlayerCharactersOnChunkStateIdentifier);
                    break;
                case GameStateIdentifierMap.SpawnPlayerCharactersOnChunkStateIdentifier:
                    ChangeGameState(GameStateIdentifierMap.SpawnChunksEnemiesStateIdentifier);
                    break;
                case GameStateIdentifierMap.SpawnChunksEnemiesStateIdentifier:
                    ChangeGameState(GameStateIdentifierMap.GameInProgressStateIdentifier);
                    break;
                case GameStateIdentifierMap.ChangeChunkStateIdentifier:
                    if (isBiomEnd)
                    {
                        ChangeGameState(GameStateIdentifierMap.ClearChunksAndBiomStateIdentifier);
                        Debug.LogWarning("here we need clear current biom and change it");
                        isBiomEnd = false;
                    }
                    else
                    {
                        ChangeGameState(GameStateIdentifierMap.SpawnChunksEnemiesStateIdentifier);
                    }
                    break;
                case GameStateIdentifierMap.ClearChunksAndBiomStateIdentifier:
                    ChangeGameState(GameStateIdentifierMap.LoadBiomStateIdentifier);
                    break;

                // Travel states
                case GameStateIdentifierMap.SpawnPlayerCharactersInTravelBiomStateIdentifier:
                    ChangeGameState(GameStateIdentifierMap.SpawnEnemiesInTravelBiomStateIdentifier);
                    break;
                case GameStateIdentifierMap.SpawnEnemiesInTravelBiomStateIdentifier:
                    ChangeGameState(GameStateIdentifierMap.GameInProgressStateIdentifier);
                    break;
                case GameStateIdentifierMap.ChangeTravelPointStateIdentifier:
                    if (isBiomEnd)
                    {
                        ChangeGameState(GameStateIdentifierMap.ClearTravelBiomStateIdentifier);
                        Debug.LogWarning("here we need clear current biom and change it");
                        isBiomEnd = false;
                    }
                    else
                    {
                        ChangeGameState(GameStateIdentifierMap.GameInProgressStateIdentifier);
                    }
                    break;
                case GameStateIdentifierMap.ClearTravelBiomStateIdentifier:
                    ChangeGameState(GameStateIdentifierMap.LoadBiomStateIdentifier);
                    break;
            }
        }
    }
}