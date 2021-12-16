import React, { Fragment, useEffect, useState } from 'react';
import axios from 'axios';
import { Container, Table } from 'semantic-ui-react';
import NavBar from './app/layout/NavBar';
import { Route } from 'react-router-dom';
import HomePage from './app/features/home/HomePage';
import LoginForm from './app/features/LoginForm';

function App() {
  const [ships, setShips] = useState([]);
  
  useEffect(() => {
    axios.get('http://localhost:5000/ships').then(response => {
        setShips(response.data);
    });
  }, []);

  return (
  <Fragment>
    <Container>
      <NavBar/>
      
      <Route path='/home' component={HomePage}/>
      <Route path='/login' component={LoginForm}/>

      <Table celled>
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell >Id</Table.HeaderCell>
            <Table.HeaderCell>Velocity</Table.HeaderCell>
            <Table.HeaderCell>Size</Table.HeaderCell>
            <Table.HeaderCell>Range</Table.HeaderCell>
          </Table.Row>
        </Table.Header>

        <Table.Body>
        {ships.map((ship: any) => (
          <Table.Row key={ship.id}>
            <Table.Cell>
                {ship.id}
            </Table.Cell>
            
            <Table.Cell>
                {ship.velocity}
            </Table.Cell>
            <Table.Cell>
                {ship.size}
            </Table.Cell>
            <Table.Cell>
                {ship.range}
            </Table.Cell>
          </Table.Row>
        ))}
        </Table.Body>
      </Table>
    </Container>
  </Fragment>
  );
}

export default App;
