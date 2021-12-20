import React from 'react';
import { Container } from 'semantic-ui-react';
import NavBar from './app/layout/NavBar';
import { Route } from 'react-router-dom';
import HomePage from './app/layout/home/HomePage';
import ShipsList from './app/layout/ShipsListPage';
import RegisterPage from './app/layout/RegisterPage';
import LoginPage from './app/layout/LoginPage';

function App() {



  return (
    <>
      <Route exact path='/' component={HomePage}/>
      <Route
        path={'/(.+)'}
        render = {() => (
          <>
            <NavBar/>
            <Container>
              <Route exact path={'/ships'} component={ShipsList}/>    
              <Route exact path={'/Register'} component={RegisterPage}/>    
              <Route exact path={'/Login'} component={LoginPage}/>    
            </Container>
          </>
        )}
      />
    </>
  );
}

export default App;
