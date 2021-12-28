import React, { useEffect } from 'react';
import { Container } from 'semantic-ui-react';
import NavBar from './app/layout/components/NavBar';
import { Route } from 'react-router-dom';
import HomePage from './app/layout/pages/HomePage';
import RegisterPage from './app/layout/pages/RegisterPage';
import LoginPage from './app/layout/pages/LoginPage';
import { useStore } from './app/stores/store';
import LoadingComponent from './app/layout/components/LoadingComponents';
import { observer } from 'mobx-react-lite';
import UsersPage from './app/layout/pages/UsersPage';
import RolesPage from './app/layout/pages/RolesPage';
import ProductsPage from './app/layout/pages/ProductsPage';
import {v4 as uuid} from 'uuid';

function App() {

  const {commonStore, userStore} = useStore();

  useEffect(() => {
    if(commonStore.token){
      console.log('Token exists');
      userStore.getUser().finally(() => commonStore.setApploaded());
    } else{
      console.log('Token does not exists');
      commonStore.setApploaded();
    }
  }, [commonStore, userStore])

  if(!commonStore.appLoaded) return <LoadingComponent content='Loading app...'/>

  return (
    <>
      <Route exact path='/' component={HomePage}/>
      <Route
        path={'/(.+)'}
        render = {() => (
          <>
            <NavBar/>
            <Container>
              <Route exact path={'/admin/users'} component={UsersPage}/>    
              {/* <Route exact path={'/ships'} component={ShipsList}/> */}

              <Route exact path={'/admin/roles'} component={RolesPage}/>    
              <Route exact path={'/Products'} component={ProductsPage}/>    
              <Route exact path={'/Register'} component={RegisterPage}/>    
              <Route exact path={'/Login'} component={LoginPage}/>    
            </Container>
          </>
        )}
      />
    </>
  );
}

export default observer(App);
