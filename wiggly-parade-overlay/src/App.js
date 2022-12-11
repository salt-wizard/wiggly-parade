import React, { useState, useCallback, useEffect } from 'react';
import EXT_CONFIG from './extConfig'
import './App.css';

const App = () => {

  var wigglies = EXT_CONFIG.emotes;

  const parade = {
    backgroundColor: "rgba(0, 0, 0, 0)",
    display: "flex",
    flexDirection: "row",
    position: "fixed",
    bottom: 0,
    width: "100%",
    animation: "infinite",
    animationName: "parade",
    animationDuration: EXT_CONFIG.duration,
    animationDelay: EXT_CONFIG.delay,
    animationTimingFunction: "linear",
  }
  

  return (
    <div className="overflow">
      <div className="Parade" style={parade}>
          {wigglies.map((wiggly, i) => {
            return (<img src={wiggly.imageUrl} key={wiggly.id + i} />)
          })}
      </div>
    </div>
  );
}

export default App;
