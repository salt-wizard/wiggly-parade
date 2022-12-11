import React from 'react';
import EXT_CONFIG from './extConfig';
import './App.css';
import styled, { keyframes } from 'styled-components';

var emotes = EXT_CONFIG.emotes;
var duration = EXT_CONFIG.duration;
var delay = EXT_CONFIG.delay;

var start = 'translateX('+window.innerWidth+'px)';
var ctrans = 'translateX(-'+(emotes.length + 1) * 56+'px)';

var march = keyframes`
  0% {
    transform: ${start};
  }
  100% {
    transform: ${ctrans};
  }
`;

var Parade = styled.div`
  background-color: rgba(0, 0, 0, 0);
  display: flex;
  flex-direction: row;
  position: fixed;
  padding: 0;
  margin: 0;
  bottom: 0;
  transform: ${ctrans};
  animation-name: ${march};
  animation-duration: ${duration};
  animation-delay: ${delay};
  animation-timing-function: linear;
`;

const App = () => {
  return (
      <Parade>
          {emotes.map((emote, i) => {
            return (<img src={emote.imageUrl} alt={emote.name} key={emote.name + i} />)
          })}
      </Parade>
  );
}

export default App;
