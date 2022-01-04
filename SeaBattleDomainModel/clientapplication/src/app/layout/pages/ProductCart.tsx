import { runInAction } from "mobx";
import { observer } from "mobx-react-lite";
import React, { SyntheticEvent, useState } from "react";
import { Button, Container, Divider, Form, Grid, Header, Icon, Input, Item, Label, Radio, Rail, Segment, Statistic, Sticky } from "semantic-ui-react";
import { setFlagsFromString } from "v8";
import { OrderItem } from "../../models/orderItem";
import { useStore } from "../../stores/store";
import OrderListItem from "../components/OrderListItem";
import './gridCustomStyles.css';



 function ProductCart(){
    const {orderItemStore} = useStore()
    const {orderItemStore: {orderItems}} = useStore()

    if(orderItems.length < 1 ){
        return (
            <Header>You did not select any product from list</Header>
        )
    }


    return(
        <Grid columns={2}>
            <Grid.Column>
                <Header> Your products</Header>
                <Item.Group>
                    {orderItems.map((item) => (
                           <OrderListItem item={item}/>
                    ))}
                    
                </Item.Group>

                <Rail position='right'>
                    <Sticky offset={50} >
                    <Segment textAlign="center" inverted color="olive">
                        <Header as='h3'>Сумма заказа</Header>
                        <Statistic size="small">
                            <Statistic.Value content={`${orderItemStore.getTotalCost()} UAH`}/>
                            <Statistic.Label content='Total cost'/>
                        </Statistic>
                        <Divider/>
                        <Button positive content='Оформить заказ'/>
                    </Segment>
                    </Sticky>
                </Rail>
            </Grid.Column>
        </Grid>
    )

    
}

export default observer(ProductCart)
