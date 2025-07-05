export const getBorderColor = (entity) => {
  if (entity?.owner === 0) {
    return 'var(--player-red)';
  } else {
    return 'var(--player-blue)';
  }
};

export const getHpTextColor = (entity) => {
  if (entity?.damageTokens > 0) {
    return 'var(--text-damage)';
  } else if (entity?.hp?.modifier > 0) {
    return 'var(--text-buff)';
  }
  return 'var(--text-white)';
};

export const getStatColor = (stat) => {
  if (stat?.modifier === 0) {
    return 'var(--text-white)';
  } else if (stat?.modifier > 0) {
    return 'var(--text-buff)';
  } else if (stat?.modifier < 0) {
    return 'var(--text-damage)';
  }
  return 'var(--text-white)';
}; 