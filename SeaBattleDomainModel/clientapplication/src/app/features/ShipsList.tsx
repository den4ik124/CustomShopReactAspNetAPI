import axios from "axios";
import React, { useEffect, useState } from "react";
import { Table } from "semantic-ui-react";

export default function ShipList(){
    const [ships, setShips] = useState([]);
    useEffect(() => {
        axios.get('http://localhost:5000/ships').then(response => {
            setShips(response.data);
        });
        }, []);

    return(
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
    )
}