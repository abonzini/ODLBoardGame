namespace ODLGameEngine
{
    public partial class GameStateMachine // Deals with triggers and interaction entry points
    {
        // INTERACTIONS, THIS IS THE ENTRY POINT OF WHERE THE INTERACTION IS REACHED, AND WILL EXECUTE CHAIN OF EFFECTS IF THE ACTIVATED ENTITY HAS THEM
        /// <summary>
        /// Processes the given interactions of a given entity
        /// </summary>
        /// <param name="inter">What interaction</param>
        /// <param name="specificContext">Specific context of that interaction</param>
        void TRIGINTER_ProcessInteraction(InteractionType inter, EffectContext specificContext)
        {
            if (specificContext.ActivatedEntity.Interactions != null && specificContext.ActivatedEntity.Interactions.TryGetValue(inter, out List<Effect> effects)) // This entity has an interaction of this type
            {
                EFFECTS_ProcessEffects(effects, specificContext);
            }
        }

        // TRIGGERS, THIS IS THE AREA OF TRIGGER ENTRY POINTS
        /// <summary>
        /// Regiters entitity's trigger specifically in one location, to be done in manual steps in different ocasion
        /// </summary>
        /// <param name="absoluteLocation">Place to register (if unit has trigger)</param>
        /// <param name="relativeLocation">Relative location according to entity</param>
        /// <param name="entity">Trigger-owning enitity</param>
        void TRIGINTER_VerifyEntityAndRegisterTriggers(BoardElement absoluteLocation, EffectLocation relativeLocation, LivingEntity entity)
        {
            if (entity.Triggers != null) // Check the entity's triggers
            {
                if (entity.Triggers.TryGetValue(relativeLocation, out Dictionary<TriggerType, List<Effect>> foundTrigger)) // See if entity has a trigger for this location
                {
                    foreach (TriggerType triggerType in foundTrigger.Keys) // Add all the corresponding triggers to this location
                    {
                        ENGINE_SubscribeTrigger(absoluteLocation, triggerType, entity.UniqueId, relativeLocation);
                    }
                }
            }
        }
        /// <summary>
        /// Registers all absolute triggers from an entity (i.e. the clearly defined locations for trigger subscription)
        /// </summary>
        /// <param name="entity">Entity to add</param>
        void TRIGINTER_RegisterEntityAbsoluteTriggers(LivingEntity entity)
        {
            // These are the places that would serve as "absolute" trigger locations
            EffectLocation[] absoluteTriggerLocations = [EffectLocation.BOARD, EffectLocation.PLAINS, EffectLocation.FOREST, EffectLocation.MOUNTAIN];
            foreach (EffectLocation location in absoluteTriggerLocations)
            {
                BoardElement place = location switch
                {
                    EffectLocation.BOARD => DetailedState.BoardState,
                    EffectLocation.PLAINS => DetailedState.BoardState.PlainsLane,
                    EffectLocation.FOREST => DetailedState.BoardState.ForestLane,
                    EffectLocation.MOUNTAIN => DetailedState.BoardState.MountainLane,
                    _ => throw new Exception("Not a valid absolute location for triggers")
                };
                TRIGINTER_VerifyEntityAndRegisterTriggers(place, location, entity); // Registers it (if need to)
            }
        }
        /// <summary>
        /// Deregisters an expired entity trigger from a board element during cleanup or movement
        /// </summary>
        /// <param name="absoluteLocation">Location to remove trigger</param>
        /// <param name="trigger">Trigger to remove</param>
        /// <param name="relativeLocation">Relative data to remove of trigger</param>
        /// <param name="entityId">Id of entity to remove</param>
        void TRIGINTER_DeregisterSpecificEntityTriggers(BoardElement absoluteLocation, TriggerType trigger, EffectLocation relativeLocation, int entityId)
        {
            ENGINE_UnsubscribeTrigger(absoluteLocation, trigger, entityId, relativeLocation);
        }
        /// <summary>
        /// Processes triggers for a specific place and trigger type, if any
        /// </summary>
        /// <param name="trigger">Trigger that was activated</param>
        /// <param name="place">Location where trigger was activated</param>
        /// <param name="specificContext">Specific context of trigger</param>
        void TRIGINTER_ProcessTrigger(TriggerType trigger, BoardElement place, EffectContext specificContext)
        {
            SortedSet<Tuple<int, EffectLocation>> triggerList = place.GetSubscribedTriggers(trigger);
            if (triggerList != null) // Some triggers were found
            {
                // Clone triggers, this is important as if in a trigger chain, the entity dies, the trigger will still be here until deleted so I can't iterate on a variable list!
                List<Tuple<int, EffectLocation>> allPossibleTriggers = triggerList.ToList();
                foreach (Tuple<int, EffectLocation> nextEntity in allPossibleTriggers)
                {
                    // Ensure that a unit doesn't trigger itself if it's also the actor
                    if (specificContext.Actor == null || nextEntity.Item1 != specificContext.Actor.UniqueId)
                    {
                        // Check if entity is is still alive
                        if (DetailedState.EntityData.TryGetValue(nextEntity.Item1, out LivingEntity entity))
                        {
                            specificContext.ActivatedEntity = entity; // This entity will be the owner of this trigger context (regardless of action)
                            EFFECTS_ProcessEffects(entity.Triggers[nextEntity.Item2][trigger], specificContext); // Find effects in unit
                        }
                        else // Entity is dead, need to clean this from the place so it never triggers again
                        {
                            TRIGINTER_DeregisterSpecificEntityTriggers(place, trigger, nextEntity.Item2, nextEntity.Item1);
                        }
                    }
                }
            }
        }
    }
}
