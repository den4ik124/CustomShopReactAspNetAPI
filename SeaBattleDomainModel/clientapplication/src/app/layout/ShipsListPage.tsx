import { observer } from "mobx-react-lite";
import React, { useEffect, useState } from "react";
import { useHistory } from "react-router-dom";
import { Button, Container, Table } from "semantic-ui-react";
import agent from "../api/agent";
import { Ship } from "../models/ship";
import { useStore } from "../stores/store";
import LoadingComponent from "./LoadingComponents";

function ShipList(){
    const history = useHistory();
    const {userStore} = useStore();
    const [ships, setShips] = useState<Ship[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        agent.Ships.list().then(response => {
          setShips(response);
          setLoading(false);
        })
    }, [])

    //TODO: testCode. Should be removed
    const {demoStore} = useStore();

    if(loading) return <LoadingComponent content="Loading app"/>

    return(

          <Container>
            <h2>{demoStore.message}</h2>
            <Button content="Click me!" onClick={demoStore.setMessage}/>


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
        {ships.map((ship: Ship) => (
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

    )
}

export default  observer(ShipList);