import React, { useEffect, useState } from 'react';
import logo from './logo.svg';
import './App.css';
// import { ducks } from './demo';
// import DuckItem from './DuckItem';
import axios from 'axios';

function App() {
  const [ships, setShips] = useState([]);
  
  useEffect(() => {
    axios.get('http://localhost:5000/ships').then(response => {
        setShips(response.data);
    });
  }, []);

  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />

        <table>
          <thead>
            <tr>
              <th>
                Ships data
              </th>
            </tr>
            <tr>
              <th>Id</th>
              <th>Velocity</th>
              <th>Size</th>
              <th>Range</th>
            </tr>
          </thead>
          <tbody>
          {ships.map((ship: any) => (
            <tr key={ship.id}>
                <td>{ship.id}</td>
                <td>{ship.velocity}</td>
                <td>{ship.size}</td>
                <td>{ship.range}</td>
             </tr>  
          ))}
          </tbody>
        </table>   
      </header>
    </div>
  );
}

export default App;
