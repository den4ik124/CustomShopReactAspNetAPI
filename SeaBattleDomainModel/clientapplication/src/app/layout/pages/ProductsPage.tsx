import { observer } from "mobx-react-lite";
import React, { Fragment, useEffect, useState } from "react";
import { Item, Button, Label,  Container, Header} from "semantic-ui-react";
import agent from "../../api/agent";
import DeleteButton from "../../common/DeleteButton";
import EditButton from "../../common/EditButton";
import { Product } from "../../models/product";
import { useStore } from "../../stores/store";
import LoadingComponent from "../components/LoadingComponents";
import CreateNewProduct from "./Modals/CreateNewProduct";

function ProductsPage(){
    const [products, setProducts] = useState<Product[]>([]);
    const [loading, setLoading] = useState(true);
    const {userStore} = useStore()
    const {productStore} = useStore()
    const [shouldUpdate, setUpdateList] = useState(false);


    if(userStore.isLoggedIn){
        var user =  userStore.user;
    }

    useEffect(() => {
        agent.Products.list().then(response => {
            setProducts(response);
          setLoading(false);
          setUpdateList(false)
        })
    }, [shouldUpdate])

function renderControllButtons(product : Product){

    if(user!.roles.includes('Manager') || user!.roles.includes('Admin')){
        return(
            <>
                <DeleteButton floated="right" onClick={() => handleRemove(product.id)}/>
                <EditButton floated='right' onClick={() => null}/>
            </>
        )
    }
}

    console.log(products);

    if(loading) return <LoadingComponent content="Loading products..."/>

    function handleRemove(id: string){
        console.log('Product has been removed...');
        productStore.removeProduct(id);
        setUpdateList(true);
    }

return(
    <Fragment>
        <Label ribbon  color="red" size="huge" content="Page is in design progress ..."/>
        {user!.roles.includes('Manager') || user!.roles.includes('Admin') ? (
            <>
                <CreateNewProduct trigger={<Button fluid positive content="Add new product"/>}/>
            </>
        ) : null}
        <Item.Group divided unstackable>
            {products.map((product) => (
                <Item key={product.title}>
                <Item.Image style={{marginRight: "50px"}} size='medium' src={`/sources/img/products/${product.title}.png`} />
                <Item.Content>
                    <Item.Header>
                        <Header content={product.title}/>
                    </Item.Header>
                    <Item.Extra>
                        <Container>
                            {renderControllButtons(product)}
                            <Label size="massive">{product.price} UAH</Label>
                            <Button positive> Buy now!</Button>
                        </Container>
                        <Container>{product.description}</Container>
                    </Item.Extra>
                </Item.Content>
                </Item>
            ))}
        </Item.Group>
    </Fragment>
)
}

export default observer(ProductsPage);