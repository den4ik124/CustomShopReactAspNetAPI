import { observer } from "mobx-react-lite";
import React, { Fragment, useEffect, useState } from "react";
import { Item, Button, Label,  Container} from "semantic-ui-react";
import agent from "../../api/agent";
import { Product } from "../../models/product";
import { useStore } from "../../stores/store";
import LoadingComponent from "../components/LoadingComponents";

function ProductsPage(){
    const [products, setProducts] = useState<Product[]>([]);
    const [loading, setLoading] = useState(true);
    const {userStore} = useStore()
    if(userStore.isLoggedIn){
        var user =  userStore.user;
        //user!.role = 'Manager'; //TODO: test code to check manager buttons
    }
    useEffect(() => {
        agent.Products.list().then(response => {
            setProducts(response);
          setLoading(false);
        })
    }, [])

    console.log(products);

    if(loading) return <LoadingComponent content="Loading products..."/>


return(
    <Fragment>
        <Label ribbon  color="red" size="huge" content="Page is in design progress ..."/>

        <Item.Group divided unstackable>

            {products.map((product) => (
                <Item key={product.title}>
                <Item.Image style={{marginRight: "50px"}} size='medium' src={`/sources/img/products/${product.title}.png`} />
                <Item.Content>
                    <Item.Header>
                        {product.title}
                        {user!.roles.includes('Manager') || user!.roles.includes('Admin') ?
                        (
                            <Item.Extra>
                                <Label ribbon color="red" content="For manages only"/> 
                                <Button negative floated='right'>Remove</Button>
                                <Button color="orange" floated='right'>Edit</Button>
                            </Item.Extra>
                        ) : (
                            <></>
                        )
                        }
                        {/* <Item.Extra>
                            <Label ribbon color="red" content="For manages only"/> 
                            <Button negative floated='right'>Remove</Button>
                            <Button color="orange" floated='right'>Edit</Button>
                        </Item.Extra> */}
                    </Item.Header>
                    <Item.Extra>
                        <Container>{product.description}</Container>
                        <Container>
                            <Label size="massive">{product.price} UAH</Label>
                            <Button positive> Buy now!</Button>
                        </Container>
                    </Item.Extra>
                </Item.Content>
                </Item>
            ))}
        </Item.Group>
    </Fragment>
)
}

export default observer(ProductsPage);